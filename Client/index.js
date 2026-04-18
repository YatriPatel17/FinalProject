const axios = require("axios");

const productApiUrl = 'http://localhost:8081/api/product';
const orderApiUrl = 'http://localhost:8082/api/order';

const readline = require('readline').createInterface({
    input: process.stdin,
    output: process.stdout
});

function askQuestion(question){
    return new Promise(resolve => readline.question(question, resolve));
}

async function main() {
    console.log('\n||=============================================================');
    console.log('\n||======    PRODUCT & ORDER MANAGEMENT MICROSERVICES    =======');
    console.log('\n||=============================================================');

    await checkServices();

    while (true) {
        console.log('\n|----------------------------------------------------------|');
        console.log('|----------             MAIN MENU                 ---------|');
        console.log(' 1. View All Products ');
        console.log(' 2. Create New Product ');
        console.log(' 3. Create Order (Service to service communication)');
        console.log(' 4. View All Orders ');
        console.log(' 5. View Order Details ');
        console.log(' 6. Update Order Status ');
        console.log(' 7. Cancel Order ');
        console.log(' 8. Exit ');
        console.log('|----------------------------------------------------------|');

        const choice = await askQuestion('\n Select Option ');

         switch (choice) {
            case '1': await viewAllProducts(); break;
            case '2': await createProduct(); break;
            case '3': await createOrder(); break;
            case '4': await viewAllOrders(); break;
            case '5': await viewOrderDetails(); break;
            case '6': await updateOrderStatus(); break;
            case '7': await cancelOrder(); break;
            case '8':
                console.log('\n Goodbye! ');
                readline.close();
                return;
            default:
                console.log(' ❌ Invalid option');
        }
    }
}

async function checkServices() {
    console.log('Checking service is available or not...\n');

    try{
        await axios.get('http://localhost:8081/health');
        console.log(' Product Service: Running on port 8081');
    }
    catch (error){
        console.log(' Product Service: Not Running (cd ProductService && dotnet run)');
    }
    
    try{
        await axios.get('http://localhost:8082/health');
        console.log(' Order Service: Running on port 8082');
    }
    catch (error){
        console.log(' Order Service: Not Running (cd OrderService && dotnet run)');
    }
    
    console.log('\n Swagger URLs:');
    console.log('   Product: http://localhost:8081/swagger');
    console.log('   Order:   http://localhost:8082/swagger\n');
}

async function viewAllProducts() {
    console.log('\n ALL PRODUCTS\n');

    try{
        const response = await axios.get(productApiUrl);
        const products = response.data;
        
        if (products.length === 0) {
            console.log('No products found!. Create one Product first!');
            return;
        }

        console.log(`${'ID'.padEnd(5)} ${'Name'.padEnd(30)} ${'Price'.padEnd(12)} ${'Stock'.padEnd(8)} ${'Category'.padEnd(15)}`);
        console.log('-'.repeat(75));
    
        products.forEach(p => {
        console.log(`${String(p.id).padEnd(5)} ${p.name.padEnd(30)} $${String(p.price).padEnd(11)} ${String(p.productQuantity).padEnd(8)} ${(p.productCategory || 'N/A').padEnd(15)}`);
        });
    } catch (error) {
    console.error('Error fetching products:', error.message);
    }
}

async function createProduct() {
    console.log('\n CREATE NEW PRODUCT\n');

    const name = await askQuestion('Name: ');
    const description = await askQuestion('Description: ');
    const price = await askQuestion('Price: ');
    const quantity = await askQuestion('Stock Quantity: ');
    const category = await askQuestion('Category: ');

    const product = {
    name: name,
        description: description,
        price: parseFloat(price),
        productQuantity: parseInt(quantity),
        productCategory: category
    };

    try {
        const response = await axios.post(productApiUrl, product);
        console.log(`\n Product created successfully! ID: ${response.data.id}`);
    } catch (error) {
        console.error('Error:', error.response?.data || error.message);
    }
}

async function createOrder() {
    console.log('\n CREATE ORDER');
    console.log('\n|--------------------------------------------------------|');
    console.log('This demonstrates SERVICE-TO-SERVICE communication:');
    console.log(' Client → Order Service → Product Service → Order Service → Client ');
    console.log('\n|--------------------------------------------------------|');

    await viewAllProducts();

    const customerName = await askQuestion('\nCustomer Name: ');
    const customerEmail = await askQuestion('Customer Email Id: ');
    
    const items = [];

    while (true) {
        const productId = await askQuestion('\nEnter Product ID (or 0 to finish): ');
        if (productId === '0') 
            break;
        
        const quantity = await askQuestion('Quantity: ');
        
        items.push({
            productId: parseInt(productId),
            quantity: parseInt(quantity)
        });
        
        console.log(` Added product ${productId} x${quantity}`);
    }

    if (items.length === 0) {
        console.log('No items added to order!');
        return;
    }
    
    const order = {
        customerName: customerName,
        customerEmail: customerEmail,
        items: items
    };

    console.log('\n Processing order...');
    console.log('Calling Product Service to validate products.');
    console.log('Calling Product Service to update stock.');

        try {
        const response = await axios.post(orderApiUrl, order);
        console.log(`\n ORDER CREATED SUCCESSFULLY!`);
        console.log(`Order ID: ${response.data.id}`);
        console.log(`Customer: ${response.data.customerName}`);
        console.log(`Total: $${response.data.totalAmount}`);
        console.log(`Status: ${response.data.status}`);
        console.log(`\nStock has been automatically updated in Product Service!`);
    } catch (error) {
        console.error(`\nOrder failed:`, error.response?.data || error.message);
    }
}

async function viewAllOrders() {
    console.log('\n All ORDERS\n');
    
    try {
        const response = await axios.get(orderApiUrl);
        const orders = response.data;
        
        if (orders.length === 0) {
            console.log('No orders found!');
            return;
        }
        
        console.log(`${'ID'.padEnd(5)} ${'Customer'.padEnd(25)} ${'Date'.padEnd(25)} ${'Total'.padEnd(12)} ${'Status'.padEnd(15)}`);
        console.log('-'.repeat(85));
        
        orders.forEach(o => {
            const date = new Date(o.orderDate).toLocaleString();
            console.log(`${String(o.id).padEnd(5)} ${o.customerName.padEnd(25)} ${date.padEnd(25)} $${String(o.totalAmount).padEnd(11)} ${o.status.padEnd(15)}`);
        });
    } catch (error) {
        console.error(' Error:', error.message);
    }
}

async function viewOrderDetails() {
    const id = await askQuestion('\nEnter Order ID: ');

    try {
        const response = await axios.get(`${orderApiUrl}/${id}`);
        const order = response.data;
        
        console.log(`\nORDER DETAILS`);
        console.log('='.repeat(60));
        console.log(`Order ID:    ${order.id}`);
        console.log(`Customer:    ${order.customerName}`);
        console.log(`Email:       ${order.customerEmail}`);
        console.log(`Date:        ${new Date(order.orderDate).toLocaleString()}`);
        console.log(`Status:      ${order.status}`);
        console.log(`Total:       $${order.totalAmount}`);
        
        if (order.items && order.items.length > 0) {
            console.log('\n ITEMS:');
            console.log(`${'Product'.padEnd(30)} ${'Qty'.padEnd(8)} ${'Unit Price'.padEnd(12)} ${'Subtotal'.padEnd(10)}`);
            console.log('-'.repeat(60));
            
            order.items.forEach(item => {
                console.log(`${item.productName.padEnd(30)} ${String(item.quantity).padEnd(8)} $${String(item.unitPrice).padEnd(11)} $${item.subtotal}`);
            });
        }
    } catch (error) {
        console.error('Error:', error.response?.data?.message || error.message);
    }
}

async function updateOrderStatus() {
    await viewAllOrders();
    const id = await askQuestion('\nEnter Order ID: ');
    console.log('\nStatus options: Pending, Confirmed, Shipped, Delivered');
    const status = await askQuestion('New Status: ');
    
    try {
        const response = await axios.put(`${orderApiUrl}/${id}/status`, { status: status });
        console.log(`\nOrder ${id} status updated to '${response.data.status}'`);
    } catch (error) {
        console.error('Error:', error.response?.data?.message || error.message);
    }
}

async function cancelOrder() {
    await viewAllOrders();
    
    const id = await askQuestion('\nEnter Order ID to cancel: ');
    const confirm = await askQuestion(`Cancel order ${id}? (y/n): `);
    
    if (confirm.toLowerCase() !== 'y') return;
    
    try {
        await axios.delete(`${orderApiUrl}/${id}`);
        console.log(`\nOrder ${id} cancelled! Stock restored to Product Service.`);
    } catch (error) {
        console.error('Error:', error.response?.data?.message || error.message);
    }
}

main().catch(console.error);